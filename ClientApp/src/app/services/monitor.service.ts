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

import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

/*************************** DATA TYPES **************************************/

export interface MonitorWhat {
    id: string;
    data?: any;
}

export enum MonitorWhatResultType {
    Success = 0,
    Failure = 1
}
export interface SuccessfulMonitorWhat extends MonitorWhat {
    type: MonitorWhatResultType.Success;
}
export interface FailedMonitorWhat extends MonitorWhat {
    type: MonitorWhatResultType.Failure;
}

export type MonitorWhatResult = SuccessfulMonitorWhat | FailedMonitorWhat;

export interface InitiatedMonitorWhat extends MonitorWhat {
    success$: Observable<MonitorWhatResult>;
    failure$: Observable<MonitorWhatResult>;
}

/*************************** SERVICE BELOW ***********************************/

@Injectable({
    providedIn: 'root'
})
export class MonitorService {

    constructor() {}

    startMonitor(monitorWhat: MonitorWhat): InitiatedMonitorWhat {
        throw new Error("Method not implemented.");
    }

    sendSuccess(monitorWhat: MonitorWhat) {
        throw new Error("Method not implemented.");
    }
    sendFailure(monitorWhat: MonitorWhat) {
        throw new Error("Method not implemented.");
    }

    getRunning(monitorWhatId: string): InitiatedMonitorWhat | undefined {
        throw new Error("Method not implemented.");
    }
    getLastResult(monitorWhatId: string, archiveData: boolean = false): MonitorWhatResult | undefined {
        throw new Error("Method not implemented.");
    }
}
