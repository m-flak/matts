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
import { AsyncSubject, Observable, filter, tap } from "rxjs";

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
    private _monitorMap: Map<string, AsyncSubject<MonitorWhatResult>> = new Map();
    private _initMap: Map<string, InitiatedMonitorWhat> = new Map();
    private _resultMap: Map<string, MonitorWhatResult> = new Map();
    private _archiveData: boolean = false;

    get archiveData() {
        return this._archiveData;
    }
    set archiveData(val: boolean) {
        this._archiveData = val;
    }

    constructor() {}

    startMonitor(monitorWhat: MonitorWhat): InitiatedMonitorWhat {
        const dupe = this._monitorMap.get(monitorWhat.id);
        if (dupe !== undefined && dupe.closed === false) {
            throw new Error(`Monitor Error: '${monitorWhat.id}' is currently being monitored.`);
        }

        const eventStream = new AsyncSubject<MonitorWhatResult>();
        this._monitorMap.set(monitorWhat.id, eventStream);

        const initiated = {
            ...monitorWhat,
            success$: eventStream.pipe(
                filter(event => event.type === MonitorWhatResultType.Success),
                tap({ complete: () => {
                    this._initMap.delete(monitorWhat.id);
                    this._monitorMap.delete(monitorWhat.id);
                }})
            ),
            failure$: eventStream.pipe(
                filter(event => event.type === MonitorWhatResultType.Failure),
                tap({ complete: () => {
                    this._initMap.delete(monitorWhat.id);
                    this._monitorMap.delete(monitorWhat.id);
                }})
            )
        };

        this._initMap.set(monitorWhat.id, initiated);
        return initiated;
    }

    // useData will be the data sent to the observer and archived
    sendSuccess(monitorWhat: MonitorWhat, useData?: any) {
        this._sendResult(MonitorWhatResultType.Success, monitorWhat, useData);
        this._saveResultData(MonitorWhatResultType.Success, monitorWhat, useData);
    }
    sendFailure(monitorWhat: MonitorWhat, useData?: any) {
        this._sendResult(MonitorWhatResultType.Failure, monitorWhat, useData);
        this._saveResultData(MonitorWhatResultType.Failure, monitorWhat, useData);
    }

    getRunning(monitorWhatId: string): InitiatedMonitorWhat | undefined {
        return this._initMap.get(monitorWhatId);
    }
    getLastResult(monitorWhatId: string): MonitorWhatResult | undefined {
        return this._resultMap.get(monitorWhatId);
    }

    private _makeResult = (
        monitorWhat: MonitorWhat, 
        resultType: MonitorWhatResultType, 
        resultData?: any
    ): MonitorWhatResult => ({...monitorWhat, type: resultType, data: resultData ?? monitorWhat.data});

    private _sendResult(type: MonitorWhatResultType, monitorWhat: MonitorWhat, useData?: any)  {

        const eventStream = this._monitorMap.get(monitorWhat.id);
        if(eventStream === undefined || (eventStream  !== undefined && eventStream.closed)) {
            console.error(`Monitor Error: '${monitorWhat.id}' does not exist or is closed.`);
            return;
        }

        eventStream.next(this._makeResult(monitorWhat, type, useData));
        eventStream.complete();
    }

    private _saveResultData(type: MonitorWhatResultType, monitorWhat: MonitorWhat, useData?: any) {

        let result = this._makeResult(monitorWhat, type, useData);
        if (!this.archiveData) {
            result.data = undefined;
        }
        this._resultMap.set(monitorWhat.id, result);
    }
}
