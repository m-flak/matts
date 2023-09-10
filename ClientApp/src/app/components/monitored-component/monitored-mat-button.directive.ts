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

import { FailedMonitorWhat, InitiatedMonitorWhat, MonitorService, SuccessfulMonitorWhat } from "src/app/services/monitor.service";
import { MonitoredComponent } from "./monitored-component.abstract";
import { Host, Self, Optional, Directive, ElementRef, Inject } from "@angular/core";
import { MatButton } from "@angular/material/button";
import { ToastService } from "src/app/services/toast.service";

@Directive({
    selector: '[monitored-mat-button]'
})
export class MonitoredMatButtonDirective extends MonitoredComponent<MatButton> {
    oldText = '';

    constructor(
        protected monitorService: MonitorService,
        @Host() @Self() @Optional() protected monitoredComponent: MatButton | null,
        private toastService: ToastService,
        @Inject(ElementRef) private element: ElementRef
    ) {
        super(monitorService, monitoredComponent);
    }

    override preUpdate(): void {
        if (this.monitoredComponent !== null) {
            this.monitoredComponent.disabled = true;
            this.oldText = this.element.nativeElement.innerText;
            this.element.nativeElement.innerText = "Working...";
        }
    }

    protected onSuccessUpdate(successResult: SuccessfulMonitorWhat): void {
        if (this.monitoredComponent !== null) {
            this.monitoredComponent.disabled = false;
            this.element.nativeElement.innerText = this.oldText;
        }
    }

    protected onFailureUpdate(failureResult: FailedMonitorWhat): void {
        if (this.monitoredComponent !== null) {
            this.monitoredComponent.disabled = false;
            this.element.nativeElement.innerText = this.oldText;
        }
        this.toastService.show('An error ocurred, or the system might be unavailable. Please try again.', { classname: 'bg-danger text-light', delay: 15000, ariaLive: 'assertive' });
    }
}
