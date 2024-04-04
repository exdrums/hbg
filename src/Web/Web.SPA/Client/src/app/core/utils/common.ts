export function arrayType<T>(obj: T | T[]): T[] {
    return Array.isArray(obj) ? obj : [obj];
  }