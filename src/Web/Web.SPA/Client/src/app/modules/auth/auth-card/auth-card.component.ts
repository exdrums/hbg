import { Component, Input } from '@angular/core';

@Component({
  selector: 'hbg-auth-card',
  templateUrl: './auth-card.component.html',
  styleUrls: ['./auth-card.component.scss'],
})
export class AuthCardComponent {
  @Input()
  title!: string;

  @Input()
  description!: string;
}
