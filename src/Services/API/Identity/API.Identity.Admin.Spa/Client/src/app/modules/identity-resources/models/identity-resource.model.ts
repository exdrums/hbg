export interface IdentityResource {
  id?: string;
  name: string;
  displayName?: string;
  description?: string;
  enabled: boolean;
  required: boolean;
  emphasize: boolean;
  showInDiscoveryDocument: boolean;
  userClaims?: string[];
  created?: Date;
  updated?: Date;
  nonEditable?: boolean;
}
