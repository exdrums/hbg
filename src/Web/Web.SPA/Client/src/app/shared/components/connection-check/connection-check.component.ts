import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import { switchMap, tap, map } from 'rxjs';


/**
 * @deprecated Not needed
 */
@Component({
  selector: 'hbg-connection-check',
  templateUrl: './connection-check.component.html',
  styleUrls: ['./connection-check.component.scss']
})
export class ConnectionCheckComponent implements OnInit {
  private readonly config: ConfigService = inject(ConfigService);
  private readonly http: HttpClient = inject(HttpClient);
  constructor() {}

  ngOnInit() {
  }

  public identityHealth$ = this.http.get<any>(this.config.hbgidentity + "/health").pipe(
    // switchMap(x => this.http.get<any>(x.hbgidentity + "/connect/userinfo")),
    // switchMap(x => this.http.get<any>(x.hbgidentity + "/home")),
    tap(x => console.log('Identity health', x)),
    map(x => x != null)
  );
}
