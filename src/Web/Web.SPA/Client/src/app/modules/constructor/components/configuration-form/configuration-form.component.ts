import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormData } from '../../models';

@Component({
  selector: 'app-configuration-form',
  templateUrl: './configuration-form.component.html',
  styleUrls: ['./configuration-form.component.scss']
})
export class ConfigurationFormComponent implements OnInit {
  @Input() projectId: string | null = null;
  @Input() jewelryType: number | null = null;
  @Output() onConfigurationChanged = new EventEmitter<FormData>();
  @Output() onGenerateImage = new EventEmitter<FormData>();

  formData: FormData = {
    material: 'Gold',
    gemstone: 'None',
    style: 'Classic',
    finish: 'Polished',
    aspectRatio: '1:1',
    notes: ''
  };

  materials = [
    'Gold', 'White Gold', 'Rose Gold', 'Platinum',
    'Silver', 'Titanium', 'Stainless Steel'
  ];

  gemstones = [
    'None', 'Diamond', 'Ruby', 'Sapphire', 'Emerald',
    'Amethyst', 'Topaz', 'Opal', 'Pearl'
  ];

  styles = [
    'Classic', 'Modern', 'Vintage', 'Bohemian',
    'Minimalist', 'Art Deco', 'Victorian'
  ];

  finishes = [
    'Polished', 'Matte', 'Brushed', 'Hammered', 'Antiqued'
  ];

  aspectRatios = [
    { value: '1:1', text: 'Square (1:1)' },
    { value: '3:4', text: 'Portrait (3:4)' },
    { value: '4:3', text: 'Landscape (4:3)' },
    { value: '9:16', text: 'Vertical (9:16)' },
    { value: '16:9', text: 'Horizontal (16:9)' }
  ];

  ngOnInit() {
    if (this.jewelryType) {
      this.formData.jewelryType = this.jewelryType;
    }
  }

  onFieldChanged() {
    this.onConfigurationChanged.emit(this.formData);
  }

  onGenerateClick() {
    this.onGenerateImage.emit(this.formData);
  }
}
