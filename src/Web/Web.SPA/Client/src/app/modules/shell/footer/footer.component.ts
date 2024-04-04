import { Component, NgModule } from '@angular/core';

@Component({
  selector: 'hbg-footer',
  template: `
    <footer><ng-content></ng-content></footer>
  `,
  styleUrls: ['./footer.component.scss'],
})

export class FooterComponent {
}
