export interface Plan {
    planID: number;
    projectID: number;
    name: string;

    hasPlanPicture: boolean;
    picCenterX: number;
    picCenterY: number;
    picWidth: number;
    picHeight: number;
    picScale: number;
    picRotation: number;
    isReadOnly: boolean;
}