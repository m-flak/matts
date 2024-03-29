/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 **/
import { AfterContentInit, Component, ContentChildren, Input, OnDestroy, OnInit, Optional, QueryList, TemplateRef } from '@angular/core';
import { ComponentTemplateDirective } from '../directives';
import { VlpProviderService } from './vlp-provider.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'cmp-view-list-panel',
  templateUrl: './view-list-panel.component.html',
  styleUrls: ['./view-list-panel.component.scss']
})
export class ViewListPanelComponent implements OnInit, OnDestroy, AfterContentInit {
  private _itemsSubscription: Subscription | null = null;

  @Input()
  providerName = '';

  @Input()
  items: any[] = [];

  @Input()
  detailNameField = 'name';

  @Input()
  itemsName = 'Items';

  @ContentChildren(ComponentTemplateDirective) 
  templates?: QueryList<ComponentTemplateDirective> | null;

  activeTab: string = 'all';
  selectedDetails?: { name: string, details: any } | null;

  itemTemplate: TemplateRef<any> | null = null;
  detailsTemplate: TemplateRef<any> | null = null;

  constructor(
    @Optional() private providerService: VlpProviderService | null
  ) { }

  ngOnInit(): void {
    if (this.providerService !== null && this.providerName !== '') {
      const provider = this.providerService.providers.get(this.providerName);
      if (provider === undefined) {
        throw new ReferenceError(`The VlpProviderService is missing a provider with the tag '${this.providerName}'.`);
      }

      this._itemsSubscription = provider.getItemsViewData().subscribe(d => this.items = d);
    }
  }

  ngOnDestroy(): void {
    if (this._itemsSubscription !== null) {
      this._itemsSubscription.unsubscribe();
    }
  }

  ngAfterContentInit(): void {
    (this.templates as QueryList<ComponentTemplateDirective>).forEach((item) => {
      switch (item.getType()) {
        case 'item':
          this.itemTemplate = item.template;
          break;

        case 'details':
          this.detailsTemplate = item.template;
          break;

        default:
          this.itemTemplate = item.template;
          break;
      }
    });
  }

  onItemClick(item: any, detailNameField: string): void {
    this.selectedDetails = {
      name: item[detailNameField],
      details: item
    };
  }

  onItemsTabClick(): void {
    this.selectedDetails = null;
  }
}
