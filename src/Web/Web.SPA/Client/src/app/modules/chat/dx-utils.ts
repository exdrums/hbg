/**
 * DevExtreme utility functions for the chat application
 * 
 * This module provides utility functions specifically for working with
 * DevExtreme components and data operations in the context of real-time
 * SignalR updates and chat functionality.
 */

/**
 * Creates a small delay to ensure DevExtreme UI updates are processed correctly
 * This is especially important when pushing real-time updates to DataSources
 * to avoid race conditions and ensure proper UI synchronization.
 * 
 * @param ms - Number of milliseconds to delay (default: 10)
 * @returns Promise that resolves after the specified delay
 */
export function dxTimeout(ms: number = 10): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Safely pushes changes to a DevExtreme DataSource with proper error handling
 * Ensures that push operations don't fail due to timing issues or invalid data
 * 
 * @param dataSource - The DevExtreme DataSource to push changes to
 * @param changes - Array of change objects in DevExtreme format
 * @returns Promise that resolves when changes are successfully pushed
 */
export async function safePushChanges(dataSource: any, changes: any[]): Promise<void> {
    if (!dataSource || !changes || changes.length === 0) {
        return;
    }

    try {
        await dxTimeout(); // Small delay for UI synchronization
        dataSource.push(changes);
    } catch (error) {
        console.error('Failed to push changes to DataSource:', error);
        // Fallback: reload the entire data source
        try {
            await dataSource.reload();
        } catch (reloadError) {
            console.error('Failed to reload DataSource after push error:', reloadError);
        }
    }
}

/**
 * Formats a DevExtreme push change object for insert operations
 * 
 * @param data - The data object to insert
 * @param index - Optional index for insertion position
 * @returns Formatted change object for DevExtreme push
 */
export function createInsertChange(data: any, index?: number): any {
    const change: any = {
        type: 'insert',
        data
    };
    
    if (index !== undefined) {
        change.index = index;
    }
    
    return change;
}

/**
 * Formats a DevExtreme push change object for update operations
 * 
 * @param key - The key of the item to update
 * @param data - The updated data object
 * @returns Formatted change object for DevExtreme push
 */
export function createUpdateChange(key: any, data: any): any {
    return {
        type: 'update',
        key,
        data
    };
}

/**
 * Formats a DevExtreme push change object for remove operations
 * 
 * @param key - The key of the item to remove
 * @returns Formatted change object for DevExtreme push
 */
export function createRemoveChange(key: any): any {
    return {
        type: 'remove',
        key
    };
}

/**
 * Safely extracts a key from a data object using various key expressions
 * 
 * @param data - The data object
 * @param keyExpr - The key expression (string, function, or array)
 * @returns The extracted key value
 */
export function extractKey(data: any, keyExpr: string | Function | string[]): any {
    if (!data) return null;
    
    if (typeof keyExpr === 'string') {
        return data[keyExpr];
    } else if (typeof keyExpr === 'function') {
        return keyExpr(data);
    } else if (Array.isArray(keyExpr)) {
        return keyExpr.map(key => data[key]);
    }
    
    return null;
}

/**
 * Converts a DevExtreme load options object to URL query parameters
 * Useful for API calls that need to match DevExtreme's filtering/sorting format
 * 
 * @param loadOptions - DevExtreme load options
 * @returns Object with query parameters
 */
export function loadOptionsToQuery(loadOptions: any): Record<string, any> {
    const query: Record<string, any> = {};
    
    if (loadOptions.skip !== undefined) {
        query.skip = loadOptions.skip;
    }
    
    if (loadOptions.take !== undefined) {
        query.take = loadOptions.take;
    }
    
    if (loadOptions.sort) {
        query.sort = JSON.stringify(loadOptions.sort);
    }
    
    if (loadOptions.filter) {
        query.filter = JSON.stringify(loadOptions.filter);
    }
    
    if (loadOptions.group) {
        query.group = JSON.stringify(loadOptions.group);
    }
    
    if (loadOptions.totalSummary) {
        query.totalSummary = JSON.stringify(loadOptions.totalSummary);
    }
    
    if (loadOptions.groupSummary) {
        query.groupSummary = JSON.stringify(loadOptions.groupSummary);
    }
    
    return query;
}

/**
 * Validates if an object matches DevExtreme's expected data format
 * 
 * @param data - The data object to validate
 * @param requiredFields - Array of required field names
 * @returns True if valid, false otherwise
 */
export function validateDataFormat(data: any, requiredFields: string[] = []): boolean {
    if (!data || typeof data !== 'object') {
        return false;
    }
    
    return requiredFields.every(field => data.hasOwnProperty(field));
}

/**
 * Safely converts a date value to a format compatible with DevExtreme
 * 
 * @param value - The date value (string, Date, or number)
 * @returns Date object or null if invalid
 */
export function parseDate(value: any): Date | null {
    if (!value) return null;
    
    if (value instanceof Date) {
        return value;
    }
    
    if (typeof value === 'string' || typeof value === 'number') {
        const date = new Date(value);
        return isNaN(date.getTime()) ? null : date;
    }
    
    return null;
}

/**
 * Debounce function specifically designed for DevExtreme event handlers
 * Prevents excessive API calls during rapid user interactions
 * 
 * @param func - The function to debounce
 * @param wait - Wait time in milliseconds
 * @param immediate - Execute on leading edge instead of trailing
 * @returns Debounced function
 */
export function debounceDevExtreme<T extends (...args: any[]) => any>(
    func: T,
    wait: number,
    immediate: boolean = false
): (...args: Parameters<T>) => void {
    let timeout: NodeJS.Timeout | null = null;
    
    return function(this: any, ...args: Parameters<T>) {
        const later = () => {
            timeout = null;
            if (!immediate) func.apply(this, args);
        };
        
        const callNow = immediate && !timeout;
        
        if (timeout) {
            clearTimeout(timeout);
        }
        
        timeout = setTimeout(later, wait);
        
        if (callNow) {
            func.apply(this, args);
        }
    };
}

/**
 * Creates a retry mechanism for DevExtreme data operations
 * Useful for handling temporary network issues or server errors
 * 
 * @param operation - The operation to retry
 * @param maxRetries - Maximum number of retry attempts
 * @param delay - Delay between retries in milliseconds
 * @returns Promise that resolves with the operation result
 */
export async function retryOperation<T>(
    operation: () => Promise<T>,
    maxRetries: number = 3,
    delay: number = 1000
): Promise<T> {
    let lastError: any;
    
    for (let attempt = 0; attempt <= maxRetries; attempt++) {
        try {
            return await operation();
        } catch (error) {
            lastError = error;
            
            if (attempt === maxRetries) {
                throw error;
            }
            
            console.warn(`Operation failed, retrying (${attempt + 1}/${maxRetries}):`, error);
            await dxTimeout(delay * Math.pow(2, attempt)); // Exponential backoff
        }
    }
    
    throw lastError;
}

/**
 * Formats error messages for display in DevExtreme components
 * 
 * @param error - The error object or message
 * @param context - Optional context information
 * @returns Formatted error message
 */
export function formatErrorMessage(error: any, context?: string): string {
    let message = 'An unexpected error occurred';
    
    if (typeof error === 'string') {
        message = error;
    } else if (error?.message) {
        message = error.message;
    } else if (error?.error?.message) {
        message = error.error.message;
    }
    
    if (context) {
        message = `${context}: ${message}`;
    }
    
    return message;
}

/**
 * Checks if DevExtreme component is properly initialized
 * 
 * @param component - The DevExtreme component instance
 * @returns True if component is ready, false otherwise
 */
export function isComponentReady(component: any): boolean {
    return component && 
           typeof component.option === 'function' && 
           component.element && 
           component.element.length > 0;
}

/**
 * Safely gets the value from a DevExtreme component
 * 
 * @param component - The DevExtreme component
 * @param defaultValue - Default value if component is not ready
 * @returns The component value or default value
 */
export function getComponentValue(component: any, defaultValue: any = null): any {
    if (!isComponentReady(component)) {
        return defaultValue;
    }
    
    try {
        return component.option('value');
    } catch {
        return defaultValue;
    }
}

/**
 * Safely sets the value of a DevExtreme component
 * 
 * @param component - The DevExtreme component
 * @param value - The value to set
 * @returns True if successful, false otherwise
 */
export function setComponentValue(component: any, value: any): boolean {
    if (!isComponentReady(component)) {
        return false;
    }
    
    try {
        component.option('value', value);
        return true;
    } catch (error) {
        console.error('Failed to set component value:', error);
        return false;
    }
}