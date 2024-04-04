export type FullStringEnum<T extends string> = {
    [P in T]: { key: T, name?: string, description: string }
}

export const htmlOfFullEnum = (list: FullStringEnum<string>) => {
    let result: string = "<ul>";
    for (const e of Object.values(list)) {
        result = result + `<li>'${e.name ?? e.key}' - ${e.description}</li>`;
    }
    result = result + '</ul>';
    return result;
}