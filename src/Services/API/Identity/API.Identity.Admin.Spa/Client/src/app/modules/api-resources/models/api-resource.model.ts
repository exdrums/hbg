export interface ApiResource {
  id?: string;
  name: string;
  displayName?: string;
  description?: string;
  enabled: boolean;
  showInDiscoveryDocument: boolean;
  allowedAccessTokenSigningAlgorithms?: string[];
  scopes?: string[];
  userClaims?: string[];
  created?: Date;
  updated?: Date;
  lastAccessed?: Date;
  nonEditable?: boolean;
}
