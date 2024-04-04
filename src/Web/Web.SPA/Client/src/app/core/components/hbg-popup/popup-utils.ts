import { filter } from "rxjs";
import { PopupResult, ToolbarID } from "./popup-context";

export const closedByOk = <T>() => filter<PopupResult<T>>(x => x.closedBy === ToolbarID.Ok);