import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormData } from '../../models';

@Component({
  selector: 'app-constructor-config',
  templateUrl: './constructor-config.component.html',
  styleUrls: ['./constructor-config.component.scss']
})
export class ConstructorConfigComponent {
  @Input() projectId: string | null = null;
  @Input() jewelryType: number | null = null;
  @Input() loading: boolean = false;
  @Output() onConfigurationChanged = new EventEmitter<FormData>();
  @Output() onGenerateImage = new EventEmitter<FormData>();

  selectedTabIndex = 0;

  handleConfigurationChange(formData: FormData) {
    this.onConfigurationChanged.emit(formData);
  }

  handleGenerateImage(formData: FormData) {
    this.onGenerateImage.emit(formData);
  }
}
