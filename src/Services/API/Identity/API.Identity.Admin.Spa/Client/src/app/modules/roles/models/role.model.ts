export interface Role {
  id?: string;
  name: string;
  normalizedName?: string;
  description?: string;
  claims?: RoleClaim[];
  created?: Date;
  updated?: Date;
}

export interface RoleClaim {
  type: string;
  value: string;
}
