import { Directive, ElementRef, HostListener, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent } from '../base/base-component';
import { HttpClient } from '@angular/common/http';
import { filter, Observable, switchMap, take, takeUntil, tap } from 'rxjs';

@Directive({
  selector: 'img'
})
export class ImgDirective extends BaseComponent implements OnInit, OnDestroy {
  constructor( ) {
    super();
    this.onDisposing$.subscribe();
  }

  private readonly element = inject(ElementRef<HTMLImageElement>);
  private readonly http = inject(HttpClient);

  /** clear Blob memory */
  private readonly onDisposing$ = this.destroyed$.pipe(
    take(1),
    tap(() => URL.revokeObjectURL(this.element.nativeElement.src))
  );

  @Input() public readonly asyncSrc: Observable<string>;
  private readonly createBlob = () => this.asyncSrc.pipe(
    filter((value) => value !== undefined),
    switchMap((value) => this.http.get(value, { responseType: "blob" }).pipe(take(1))),
    tap((value) => {
      this.element.nativeElement.src = URL.createObjectURL(value);
    }),
    takeUntil(this.destroyed$)
  );
  
  public ngOnInit(): void {
    if (this.asyncSrc !== undefined) {
      this.createBlob().subscribe();
    }
  }
    
  	/**
     * <img>[src] changed => emit "load" event => cache the address to the lastSrc,
     * it used to empty Blob cache later
     */
	private lastSrc: string;
  @HostListener("load") private removeOnReload() {
		// remove previous cached image
		URL.revokeObjectURL(this.lastSrc);
		// ssave to remove later
		this.lastSrc = this.element.nativeElement.src;
  }
  


}
