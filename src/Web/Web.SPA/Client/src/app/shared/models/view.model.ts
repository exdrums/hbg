import { Subject } from "rxjs";

export type ViewIndex = 'v1' | 'v2' | 'v3';
export interface View<Outcoming = any, Incoming = Outcoming> {
  /** Push outcoming message here to change view and set this messabe as "viewInput" of recipient */
  viewOutput$: Subject<Outcoming>,
  /** Alternative second output for other recipient view */
  viewOutput2$?: Subject<Outcoming>,
  /** Incoming information from other views, emits messages after view was shown */
  viewInput$: Subject<Incoming>,
  /** Call this subject to go back to previous view */
  viewBack$: Subject<any>
}

export interface ViewSettings<Outcoming = any, Incoming = Outcoming> {
  /** Settings subject view */
  view: View<Outcoming, Incoming>,
  /** Bind main subject viewOutput with this view  */
  viewOutputTo?: View<any, Outcoming>,
  /** Bind secondary subject viewOutput with this view  */
  viewOutput2To?: View<any, Outcoming>,
}