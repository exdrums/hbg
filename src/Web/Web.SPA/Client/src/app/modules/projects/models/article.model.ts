export interface Article {
    articleID: number;
    projectID: number;
    planID: number;
    name: string;
    type: number /* ArticleType */
    typeAsString: string;
    x: number;
    y: number;
}