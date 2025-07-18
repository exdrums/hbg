export interface IClientAppSettings {
    hbgspa: string;
    hbgspadev: string;
    hbgidentity: string;
    // hbgidentityadmin: string;
    hbgfiles: string;
    hbgprojects: string;
    hbgemailer: string;
    hbgcontacts: string;

    // audience: string;
}

export const defaultSettings: IClientAppSettings = {
    // hbgspa: "https://spa.houbirg.local",
    // hbgidentity: "https://sts.houbirg.local",
    // hbgidentityadmin: "https://admin.houbirg.local",

    hbgspa: "https://localhost:5799",
    hbgspadev: "https://localhost:5799",
    hbgidentity: "https://localhost:5700",
    // hbgidentityadmin: "https://localhost:5798",

    hbgfiles: "http://localhost:5701",
    hbgprojects: "http://localhost:5702",
    hbgemailer: "http://localhost:5703",
    hbgcontacts: "http://localhost:5704",

    // audience: null
};