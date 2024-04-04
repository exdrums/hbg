import { MonoTypeOperatorFunction } from "rxjs";
import { filter } from "rxjs/operators";

export function notnull<T>(): MonoTypeOperatorFunction<T> {
    return filter<T>(x => x != null && x !== null);
}