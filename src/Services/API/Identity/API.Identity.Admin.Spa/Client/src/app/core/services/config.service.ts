import { Injectable } from '@angular/core';
import { defaultSettings, IClientAppSettings } from '../models/app-settings.model';
import { environment } from '@env/environment';
import { BehaviorSubject, filter, firstValueFrom } from 'rxjs';

@Injectable()
export class ConfigService implements IClientAppSettings {
  constructor() {}

  public hbgidentityadminspa: string;
  public hbgidentityadminspadev: string;
  public hbgidentity: string;
  public hbgidentityadminapi: string;

  private baseURI = environment['serverUri'] != null ?
    environment['serverUri']
    : document.baseURI.endsWith('/') ? document.baseURI : `${document.baseURI}/`;

  private url = `${this.baseURI}configuration`;
  private ready$ = new BehaviorSubject<boolean>(false);
  public isReady = () => firstValueFrom(this.ready$.pipe(filter(o => o)))

  public readonly fetchAppSettings = async () => {
    try {
      const response = await fetch(this.url);
      const obj = await response.json();
      Object.assign(this, obj);
      console.log('AppSettings loaded', this);
      this.ready$.next(true);
    }
    catch {
      Object.assign(this, defaultSettings);
      console.error("Cannot fetch AppSettings. Default values used.");
      this.ready$.next(true);
    }
  }
}
