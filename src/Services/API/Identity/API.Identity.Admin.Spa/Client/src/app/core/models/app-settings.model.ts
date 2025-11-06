export interface IClientAppSettings {
  hbgidentityadminspa: string;
  hbgidentityadminspadev: string;
  hbgidentity: string;
  hbgidentityadminapi: string;
}

export const defaultSettings: IClientAppSettings = {
  hbgidentityadminspa: 'https://localhost:5796',
  hbgidentityadminspadev: 'http://localhost:4201',
  hbgidentity: 'https://localhost:5700',
  hbgidentityadminapi: 'http://localhost:5797'
};
