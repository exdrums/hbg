import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmailerComponent } from './emailer.component';
import { TemplateListComponent } from './template-list/template-list.component';
import { TemplateEditorComponent } from './template-editor/template-editor.component';
import { SharedModule } from '@app/shared/shared.module';
import { TemplatesDataSource } from './data/templates.data-source';
import { DxHtmlEditorModule } from 'devextreme-angular';
import { DistributionListComponent } from './distribution-list/distribution-list.component';
import { DistributionsDataSource } from './data/distributions.data-source';
import { SendersDataSource } from './data/sender.data-source';
import { SenderListComponent } from './sender-list/sender-list.component';
import { ReceiversDataSource } from './data/receivers.data-source';
import { ReceiverListComponent } from './receiver-list/receiver-list.component';
import { EmailingReceiverListComponent } from './emailing-receivers-list/emailing-receiver-list.component';
import { EmailerWebSocketConnection } from './data/emailer-ws-connection.service';
import { DistProgressCellComponent } from './distribution-list/dist-progress-cell/dist-progress-cell.component';

@NgModule({
  imports: [
    SharedModule,
    DxHtmlEditorModule
  ],
  declarations: [
    EmailerComponent,
    
    DistributionListComponent,
    DistProgressCellComponent,
    TemplateListComponent,
    TemplateEditorComponent,
    SenderListComponent,
    ReceiverListComponent,
    EmailingReceiverListComponent
  ],
  providers: [
    TemplatesDataSource,
    DistributionsDataSource,
    SendersDataSource,
    ReceiversDataSource,
    EmailerWebSocketConnection
  ]
})
export class EmailerModule { }
