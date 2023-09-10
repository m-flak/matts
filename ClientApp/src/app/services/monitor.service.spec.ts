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

import { TestBed } from "@angular/core/testing";
import { InitiatedMonitorWhat, MonitorService, MonitorWhat, MonitorWhatResult, MonitorWhatResultType } from "./monitor.service";
import { lastValueFrom } from "rxjs";

const MW_ID = "my cool event";

describe('MonitorService', () => {
    let monitorService: MonitorService;

    beforeEach(() => {
        TestBed.configureTestingModule({
          declarations: [],
          imports: [],
          providers: [
            MonitorService
          ]
        });

        monitorService = TestBed.inject(MonitorService);
    });

    it('should instantiate', () => {
        expect(monitorService).toBeTruthy();
    });

    describe('When a MonitorWhat is present', () => {
        const mceContract: MonitorWhat = { id: MW_ID, data: 123 };
        let mceStarted: InitiatedMonitorWhat;

        beforeEach(() => {
            mceStarted = monitorService.startMonitor( mceContract );
        });

        it('can send successes', (done) => {
            mceStarted.success$.subscribe((result: MonitorWhatResult) => {
                expect(result.type).toBe(MonitorWhatResultType.Success);
                expect(result.id).toEqual(MW_ID);
                expect(result.data).toEqual(123);

                const running: InitiatedMonitorWhat | undefined = monitorService.getRunning(MW_ID);
                expect(running).toBeUndefined();
                done();
            });

            const running: InitiatedMonitorWhat | undefined = monitorService.getRunning(MW_ID);
            expect(running).toBeDefined();

            monitorService.sendSuccess( mceContract );
        });

        it('can send failures', (done) => {
            mceStarted.failure$.subscribe((result: MonitorWhatResult) => {
                expect(result.type).toBe(MonitorWhatResultType.Failure);
                expect(result.id).toEqual(MW_ID);
                expect(result.data).toEqual(123);

                const running: InitiatedMonitorWhat | undefined = monitorService.getRunning(MW_ID);
                expect(running).toBeUndefined();
                done();
            });

            const running: InitiatedMonitorWhat | undefined = monitorService.getRunning(MW_ID);
            expect(running).toBeDefined();

            monitorService.sendFailure( mceContract );
        });

        it('stores last results', async () => {
            monitorService.sendFailure( mceContract );
            await lastValueFrom( mceStarted.failure$ );

            const result: MonitorWhatResult | undefined = monitorService.getLastResult(MW_ID, false);
            expect(result).toBeDefined();
            expect(result?.data).toBeUndefined();
            expect(result?.type).toBe(MonitorWhatResultType.Failure);
        });

        it('stores last results with data when instructed', async () => {
            monitorService.sendSuccess( mceContract );
            await lastValueFrom( mceStarted.success$ );

            const result: MonitorWhatResult | undefined = monitorService.getLastResult(MW_ID, true);
            expect(result).toBeDefined();
            expect(result?.data).toEqual(mceContract.data);
            expect(result?.type).toBe(MonitorWhatResultType.Success);
        });
    });

    describe('When a MonitorWhat is NOT present', () => {
        const mceContract: MonitorWhat = { id: MW_ID, data: 123 };

        it('gracefully does nothing if sending a success', () => {
            expect(monitorService.sendSuccess( mceContract )).not.toThrow();
        });

        it('gracefully does nothing if sending a failure', () => {
            expect(monitorService.sendFailure( mceContract )).not.toThrow();
        });

        it('should return undefined when calling the get methods', () => {
            const running: InitiatedMonitorWhat | undefined = monitorService.getRunning(MW_ID);
            const result: MonitorWhatResult | undefined = monitorService.getLastResult(MW_ID, true);

            expect(running).toBeUndefined();
            expect(result).toBeUndefined();
        });
    });
});
