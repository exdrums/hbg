import { distinctUntilChanged } from "rxjs";

var reISO = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;
var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;

export const dateTimeReviver = (key, value) => {
  if (typeof value === 'string') {
    var a = reISO.exec(value);
    if (a)
      return new Date(value);
    a = reMsAjax.exec(value);
    if (a) {
      var b = a[1].split(/[-+,.]/);
      return new Date(b[0] ? +b[0] : 0 - +b[1]);
    }
  }
  return value;
}

export const distinctUntilChangedObj = <T>() => distinctUntilChanged<T>((a: T, b: T) => JSON.stringify(a) === JSON.stringify(b));

/**
 * Used by deserialization of Date fields
 */
export function dateReviver(key, value) {
  var a;
  if (typeof value === 'string') {
    var reDateDetect = /(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/;
    if (typeof value == 'string' && (reDateDetect.exec(value))) {
      return new Date(value);
    }
    return value;
  }
  return value;
}
