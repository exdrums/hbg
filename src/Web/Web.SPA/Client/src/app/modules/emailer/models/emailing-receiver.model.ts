import { Receiver } from "./receiver.model";

export interface EmailingReceiver extends Receiver {
    assigned: boolean;
}