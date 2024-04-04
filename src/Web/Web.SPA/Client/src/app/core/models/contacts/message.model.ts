export interface Message {
    messageID: number,
    /**Who wrote this message */
    contactParticipantID: number,
    text: string,
    /**Date string */
    sent: string,
    read: boolean,
    /**Non persistant data, initialised by server in AutoMapper, (or by client if temporary offline?) */
    owned: boolean,

    // Not part of Dto, but in Model
    contactID?: number
}

export interface Message_POST {
    // given in path
    // contactID: number,
    text: string
}

export interface Message_HUB {
    contactParticipantName: string;
    text: string;
}