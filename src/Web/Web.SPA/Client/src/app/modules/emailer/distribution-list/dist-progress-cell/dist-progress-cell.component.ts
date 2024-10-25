import { Component, inject, Input } from '@angular/core';
import { EmailerWebSocketConnection } from '../../data/emailer-ws-connection.service';

/**
 * Progress Cell for the DoistributionList Component,
 * Shows interactively the email sending progress for the entry
 */
@Component({
  selector: 'hbg-dist-progress-cell',
  templateUrl: './dist-progress-cell.component.html',
  styleUrls: ['./dist-progress-cell.component.scss']
})
export class DistProgressCellComponent {
  constructor() { }
  private readonly connection = inject(EmailerWebSocketConnection);

  @Input() set distributionId(value: number) {
    console.log('Input_distributionId', value);
    this.connection.trackDistribution(value);
    this.connection.subscribeDistributionUpdated$(value).subscribe(x => console.log('subscribeDistributionUpdated', x));
  }
}
