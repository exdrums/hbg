export interface Grant {
  id?: string;
  key: string;
  type: string;
  subjectId: string;
  clientId: string;
  creationTime: Date;
  expiration?: Date;
  data?: string;
  description?: string;
}
