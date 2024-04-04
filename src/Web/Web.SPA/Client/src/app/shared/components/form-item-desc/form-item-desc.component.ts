import { Component, ElementRef, Input, OnInit } from '@angular/core';
import { NestedOptionHost } from 'devextreme-angular';
import { DxoLabelComponent } from 'devextreme-angular/ui/nested';

@Component({
  selector: 'dxo-hbg-label',
  templateUrl: './form-item-desc.component.html',
  styleUrls: ['./form-item-desc.component.scss']
})
export class FormItemDescComponent extends DxoLabelComponent {
  constructor(parentOptionHost: NestedOptionHost, optionHost: NestedOptionHost, private ref: ElementRef) { 
    super(parentOptionHost, optionHost);
    console.log('this', this);
    // console.log('LabelElement', ref);
    // this.text = "EditedText";
    this.template = (data) => {
      console.log('TemplateIntern', data);
      return data.text;
    };
    // console.log('FormItemDescComponent', parentOptionHost, optionHost);
  }

  // @Input() public description: string;

}
