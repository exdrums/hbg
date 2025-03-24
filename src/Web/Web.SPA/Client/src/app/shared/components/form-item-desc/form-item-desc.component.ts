import { Component, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';
import { DxTemplateHost, NestedOptionHost } from 'devextreme-angular';
import { DxoLabelComponent } from 'devextreme-angular/ui/nested';

@Component({
  selector: 'dxo-hbg-label',
  templateUrl: './form-item-desc.component.html',
  styleUrls: ['./form-item-desc.component.scss']
})
export class FormItemDescComponent extends DxoLabelComponent {
  constructor(parentOptionHost: NestedOptionHost, optionHost: NestedOptionHost, renderer: Renderer2, templateHost: DxTemplateHost, element: ElementRef) { 
    super(parentOptionHost, optionHost, renderer, document, templateHost, element);
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
