/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023 Matthew E. Kehrer <matthew@kehrer.dev>
 *
 * THIS FILE ORIGINALLY: Toast Demo Code from https://github.com/ng-bootstrap/ng-bootstrap/tree/master/demo/src/app/components/toast/demos/howto-global
 * Copyright (c) 2019-2022 Angular ng-bootstrap team
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
import { Attribute, Component, Input, TemplateRef } from '@angular/core';

import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'cmp-toasts-container',
  template: `
		<ngb-toast
			*ngFor="let toast of toastService.toasts"
			[class]="toast.classname"
			[autohide]="true"
			[delay]="toast.delay || 5000"
			(hidden)="toastService.remove(toast)"
			[attr.aria-live]="toast?.ariaLive ?? ariaLive"
		>
      <a style="cursor: pointer; float: right" (click)="toastService.remove(toast)">X</a>
			<ng-template [ngIf]="isTemplate(toast)" [ngIfElse]="text">
				<ng-template [ngTemplateOutlet]="toast.textOrTpl"></ng-template>
			</ng-template>

			<ng-template #text>{{ toast.textOrTpl }}</ng-template>
		</ngb-toast>
	`,
  host: {
    '[attr.aria-live]': 'ariaLive',
    class: 'toast-container end-0',
    style: 'z-index: 1200; top: 4.25em',
  },
})
export class ToastsContainerComponent {
  constructor(
    @Attribute('aria-live') public ariaLive: string,
    public toastService: ToastService,
  ) {
    if (this.ariaLive == null) {
      this.ariaLive = 'status';
    }
  }

  isTemplate(toast: any) {
    return toast.textOrTpl instanceof TemplateRef;
  }
}
