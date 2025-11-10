export interface User {
  id?: string;
  username: string;
  email: string;
  emailConfirmed?: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed?: boolean;
  firstName?: string;
  lastName?: string;
  displayName?: string;
  isActive: boolean;
  lockoutEnabled: boolean;
  lockoutEnd?: Date;
  accessFailedCount?: number;
  twoFactorEnabled: boolean;
  roles?: string[];
  claims?: UserClaim[];
  created?: Date;
  updated?: Date;
}

export interface UserClaim {
  type: string;
  value: string;
}
