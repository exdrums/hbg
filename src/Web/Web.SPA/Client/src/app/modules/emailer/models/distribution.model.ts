import { EmailStatus } from "./email.model";

export interface Distribution
{
    distributionID: number;
    senderID: number; 
    templateID: number;

    name: string;
    senderName: string;
    
    templateName: string;

    status: EmailStatus;
    emailsCount: number;
}