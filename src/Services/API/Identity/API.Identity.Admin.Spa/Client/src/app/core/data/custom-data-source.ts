import DataSource from 'devextreme/data/data_source';
import { BehaviorSubject, Observable } from 'rxjs';
import { CustomDataStore } from './custom-data-store';
import { DeepPartial } from "devextreme/core";

/**
 * Custom DataSource wrapper for DevExtreme components
 * Provides reactive observables for state management
 */
export class CustomDataSource<T = any, TKey = any> extends DataSource<T> {

  private _selectedItem$ = new BehaviorSubject<T | null>(null);
  private _isLoading$ = new BehaviorSubject<boolean>(false);
  private _error$ = new BehaviorSubject<string | null>(null);
  private _items$ = new BehaviorSubject<T[]>([]);

  constructor(public customStore: CustomDataStore<T, TKey>) {
    super({
      store: customStore,
      paginate: false,
      reshapeOnPush: true
    });

    this.setupEventHandlers();
  }

  public get selectedItem$(): Observable<T | null> {
    return this._selectedItem$.asObservable();
  }

  public get isLoading$(): Observable<boolean> {
    return this._isLoading$.asObservable();
  }

  public get error$(): Observable<string | null> {
    return this._error$.asObservable();
  }

  public get items$(): Observable<T[]> {
    return this._items$.asObservable();
  }

  public get selectedItem(): T | null {
    return this._selectedItem$.value;
  }

  public setSelected(item: T | null): void {
    this._selectedItem$.next(item);
  }

  public selectByKey(key: any): void {
    const items = this.items();
    const item = items.find(i => this.getItemKey(i) === key);
    this.setSelected(item || null);
  }

  public clearSelection(): void {
    this.setSelected(null);
  }

  public async refresh(): Promise<void> {
    try {
      this._error$.next(null);
      await this.reload();
      this.updateItemsObservable();
    } catch (error) {
      this.handleError(error);
    }
  }

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

  public async remove(key: any): Promise<void> {
    try {
      this._error$.next(null);
      await this.store().remove(key);

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

  private setupEventHandlers(): void {
    this.on('loadingChanged', (isLoading: boolean) => {
      this._isLoading$.next(isLoading);
      if (!isLoading) {
        this.updateItemsObservable();
      }
    });

    this.on('loadError', (error: any) => {
      this.handleError(error);
    });

    this.on('changed', () => {
      this.updateItemsObservable();
    });
  }

  private updateItemsObservable(): void {
    const items = this.items();
    this._items$.next([...items]);
  }

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

  public override dispose(): void {
    this._selectedItem$.complete();
    this._isLoading$.complete();
    this._error$.complete();
    this._items$.complete();

    if (this.customStore && typeof (this.customStore as any).destroy === 'function') {
      (this.customStore as any).destroy();
    }

    super.dispose();
  }
}
