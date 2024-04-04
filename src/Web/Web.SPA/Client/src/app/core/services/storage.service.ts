import { Injectable } from '@angular/core';
import { dateTimeReviver } from '../utils/json-utils';

@Injectable({ providedIn: 'root' })
export class StorageService {
constructor() {
  this.storage = sessionStorage;
 }
 private storage: any;

  public retrieve<T>(key: string): T {
    const item = this.storage.getItem(key);
    if (item && item !== 'undefined') {
        return JSON.parse(this.storage.getItem(key), dateTimeReviver);
    }
    return null;
  }

  public store(key: string, value: any) {
    this.storage.setItem(key, JSON.stringify(value));
  }
}
