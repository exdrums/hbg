import DataSource from 'devextreme/data/data_source';
import { BehaviorSubject, Observable } from 'rxjs';
import { CustomDataStore } from './custom-data-store';
import { DeepPartial } from "devextreme/core";

/**
 * Custom DataSource wrapper for DevExtreme components
 * 
 * This class extends DevExtreme's DataSource to provide additional functionality:
 * - Real-time data updates through SignalR
 * - Enhanced selection management
 * - Loading state tracking
 * - Error handling and recovery
 * - Integration with reactive patterns (RxJS)
 * 
 * The class serves as a bridge between DevExtreme's data layer and our
 * real-time SignalR communication system, providing a consistent interface
 * for all data-driven components in the chat application.
 */
export class CustomDataSource<T = any> extends DataSource<T> {
    
    private _selectedItem$ = new BehaviorSubject<T | null>(null);
    private _isLoading$ = new BehaviorSubject<boolean>(false);
    private _error$ = new BehaviorSubject<string | null>(null);
    private _items$ = new BehaviorSubject<T[]>([]);
    
    constructor(public customStore: CustomDataStore<T>) {
        super({
            store: customStore,
            paginate: false, // We handle our own pagination for real-time updates
            reshapeOnPush: true // Important for real-time updates
        });
        
        this.setupEventHandlers();
    }

    /**
     * Observable for the currently selected item
     */
    public get selectedItem$(): Observable<T | null> {
        return this._selectedItem$.asObservable();
    }

    /**
     * Observable for loading state
     */
    public get isLoading$(): Observable<boolean> {
        return this._isLoading$.asObservable();
    }

    /**
     * Observable for error state
     */
    public get error$(): Observable<string | null> {
        return this._error$.asObservable();
    }

    /**
     * Observable for items array
     */
    public get items$(): Observable<T[]> {
        return this._items$.asObservable();
    }

    /**
     * Get the currently selected item
     */
    public get selectedItem(): T | null {
        return this._selectedItem$.value;
    }

    /**
     * Check if data source is currently loading
     */
    public get isLoadingg(): boolean {
        return this._isLoading$.value;
    }

    /**
     * Get current error message if any
     */
    public get error(): string | null {
        return this._error$.value;
    }

    //#region Selection Management

    /**
     * Set the selected item
     */
    public setSelected(item: T | null): void {
        this._selectedItem$.next(item);
    }

    /**
     * Select an item by its key
     */
    public selectByKey(key: any): void {
        const items = this.items();
        const item = items.find(i => this.customStore.key() === key);
        this.setSelected(item || null);
    }

    /**
     * Clear the current selection
     */
    public clearSelection(): void {
        this.setSelected(null);
    }

    /**
     * Get the selected item (alias for compatibility)
     */
    public selected(): T | null {
        return this.selectedItem;
    }

    //#endregion

    //#region Data Operations

    /**
     * Refresh data from the server
     */
    public async refresh(): Promise<void> {
        try {
            this._error$.next(null);
            await this.reload();
            this.updateItemsObservable();
        } catch (error) {
            this.handleError(error);
        }
    }

    /**
     * Insert a new item
     */
    public async insert(item: T): Promise<T> {
        try {
            this._error$.next(null);
            const result = await this.store().insert(item);
            this.updateItemsObservable();
            return result;
        } catch (error) {
            this.handleError(error);
            throw error;
        }
    }

    /**
     * Update an existing item
     */
    public async update(key: any, values: DeepPartial<T>): Promise<void> {
        try {
            this._error$.next(null);
            await this.store().update(key, values);
            this.updateItemsObservable();
        } catch (error) {
            this.handleError(error);
            throw error;
        }
    }

    /**
     * Remove an item
     */
    public async remove(key: any): Promise<void> {
        try {
            this._error$.next(null);
            await this.store().remove(key);
            
            // Clear selection if the removed item was selected
            const selectedItem = this.selectedItem;
            if (selectedItem && this.getItemKey(selectedItem) === key) {
                this.clearSelection();
            }
            
            this.updateItemsObservable();
        } catch (error) {
            this.handleError(error);
            throw error;
        }
    }

    //#endregion

    //#region Search and Filtering

    /**
     * Search for items matching a query
     */
    public search(query: string, fields?: string[]): T[] {
        const items = this.items();
        if (!query.trim()) return items;
        
        const searchQuery = query.toLowerCase();
        
        return items.filter(item => {
            if (fields) {
                return fields.some(field => {
                    const value = (item as any)[field];
                    return value && value.toString().toLowerCase().includes(searchQuery);
                });
            } else {
                // Search in all string fields
                return Object.values(item as any).some(value => 
                    typeof value === 'string' && value.toLowerCase().includes(searchQuery)
                );
            }
        });
    }

    /**
     * Filter items by a predicate function
     */
    public filterBy(predicate: (item: T) => boolean): T[] {
        return this.items().filter(predicate);
    }

    /**
     * Find an item by predicate
     */
    public findItem(predicate: (item: T) => boolean): T | undefined {
        return this.items().find(predicate);
    }

    /**
     * Find an item by key
     */
    public findByKey(key: any): T | undefined {
        return this.items().find(item => this.getItemKey(item) === key);
    }

    //#endregion

    //#region Utility Methods

    /**
     * Get the count of items
     */
    public count(): number {
        return this.items().length;
    }

    /**
     * Check if data source is empty
     */
    public isEmpty(): boolean {
        return this.count() === 0;
    }

    /**
     * Get the key for an item
     */
    public getItemKey(item: T): any {
        const keyExpr = this.customStore.key();
        if (typeof keyExpr === 'string') {
            return (item as any)[keyExpr];
        } else if (typeof keyExpr === 'function') {
            // @ts-ignore
            return keyExpr(item);
        }
        return null;
    }

    /**
     * Sort items by a field
     */
    public sortBy(field: string, desc: boolean = false): T[] {
        const items = [...this.items()];
        return items.sort((a, b) => {
            const aVal = (a as any)[field];
            const bVal = (b as any)[field];
            
            if (aVal < bVal) return desc ? 1 : -1;
            if (aVal > bVal) return desc ? -1 : 1;
            return 0;
        });
    }

    /**
     * Group items by a field
     */
    public groupBy(field: string): Record<string, T[]> {
        const items = this.items();
        const groups: Record<string, T[]> = {};
        
        items.forEach(item => {
            const key = (item as any)[field]?.toString() || 'undefined';
            if (!groups[key]) {
                groups[key] = [];
            }
            groups[key].push(item);
        });
        
        return groups;
    }

    //#endregion

    //#region Private Methods

    /**
     * Set up event handlers for the data source
     */
    private setupEventHandlers(): void {
        // Handle loading state changes
        this.on('loadingChanged', (isLoading: boolean) => {
            this._isLoading$.next(isLoading);
            if (!isLoading) {
                this.updateItemsObservable();
            }
        });

        // Handle load errors
        this.on('loadError', (error: any) => {
            this.handleError(error);
        });

        // Handle data changes
        this.on('changed', () => {
            this.updateItemsObservable();
        });
    }

    /**
     * Update the items observable when data changes
     */
    private updateItemsObservable(): void {
        const items = this.items();
        this._items$.next([...items]); // Create a copy to ensure change detection
    }

    /**
     * Handle errors consistently
     */
    private handleError(error: any): void {
        console.error('CustomDataSource error:', error);
        
        let errorMessage = 'An unexpected error occurred';
        
        if (typeof error === 'string') {
            errorMessage = error;
        } else if (error?.message) {
            errorMessage = error.message;
        } else if (error?.error?.message) {
            errorMessage = error.error.message;
        }
        
        this._error$.next(errorMessage);
    }

    //#endregion

    //#region Cleanup

    /**
     * Dispose of the data source and clean up resources
     */
    public override dispose(): void {
        this._selectedItem$.complete();
        this._isLoading$.complete();
        this._error$.complete();
        this._items$.complete();
        
        // Dispose of the custom store if it has a destroy method
        if (this.customStore && typeof (this.customStore as any).destroy === 'function') {
            (this.customStore as any).destroy();
        }
        
        super.dispose();
    }

    //#endregion
}