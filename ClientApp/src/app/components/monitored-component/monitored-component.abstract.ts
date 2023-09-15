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

import { Directive, Host, Input, OnDestroy, Optional, Self } from "@angular/core";
import { Subscription, map } from "rxjs";
import { FailedMonitorWhat, InitiatedMonitorWhat, MonitorService, SuccessfulMonitorWhat } from "src/app/services/monitor.service";

@Directive()
export abstract class MonitoredComponent<T> implements OnDestroy {
    protected initiatedMonitorWhat: InitiatedMonitorWhat | null = null;
    
    protected successSub: Subscription | null = null;
    protected failSub: Subscription | null = null;

    @Input()
    monitorWhatId: string = '';

    constructor(
        protected monitorService: MonitorService,
        protected monitoredComponent: T | null
    ) {}

    ngOnDestroy(): void {
        if (this.successSub !== null) {
            this.successSub.unsubscribe();
        }
        if (this.failSub !== null) {
            this.failSub.unsubscribe();
        }
    }

    kickoff(extraData?: any) {
        this.preUpdate();

        this.initiatedMonitorWhat = 
            this.monitorService.startMonitor({ id: this.monitorWhatId, data: extraData });
        
        this.successSub = this.initiatedMonitorWhat.success$.pipe(
            map(r => r as SuccessfulMonitorWhat)
        ).subscribe(r => this.onSuccessUpdate(r));

        this.failSub = this.initiatedMonitorWhat.failure$.pipe(
            map(r => r as FailedMonitorWhat)
        ).subscribe(r => this.onFailureUpdate(r));
    }

    protected preUpdate(): void {
        return;
    }

    protected abstract onSuccessUpdate(successResult: SuccessfulMonitorWhat): void;

    protected abstract onFailureUpdate(failureResult: FailedMonitorWhat): void;
}
