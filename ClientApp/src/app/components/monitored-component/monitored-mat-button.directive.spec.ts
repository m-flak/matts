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

import { Component } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { MonitorService, MonitorWhatResultType } from 'src/app/services/monitor.service';
import { ComponentsModule } from '../components.module';
import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { MatButtonModule } from '@angular/material/button';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonHarness } from '@angular/material/button/testing';
import { By } from '@angular/platform-browser';
import { MonitoredMatButtonDirective } from './monitored-mat-button.directive';
import { EMPTY, of } from 'rxjs';
import { ToastService } from 'src/app/services/toast.service';

const MW_ID = 'monny me';

@Component({
  selector: 'test-component',
  template: `<button mat-flat-button monitored-mat-button monitorWhatId="${MW_ID}" color="accent" class="align-self-center" type="submit">Register</button>`,
})
class TestComponent {}

describe('MonitoredMatButtonDirective', () => {
  let directive: MonitoredMatButtonDirective;
  let fixture: ComponentFixture<TestComponent>;
  let monitorService: MonitorService;
  let toastService: ToastService;

  let loader: HarnessLoader;
  let button: MatButtonHarness;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TestComponent],
      imports: [BrowserAnimationsModule, MatButtonModule, ComponentsModule],
      providers: [MonitorService, ToastService],
      teardown: { destroyAfterEach: false },
    }).compileComponents();

    monitorService = TestBed.inject(MonitorService);
    toastService = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(TestComponent);
    loader = TestbedHarnessEnvironment.loader(fixture);

    fixture.detectChanges();

    await fixture.whenStable();
    button = await loader.getHarness(MatButtonHarness);
    const directiveHost = fixture.debugElement.query(By.directive(MonitoredMatButtonDirective));
    directive = directiveHost.injector.get(MonitoredMatButtonDirective);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
    expect(directive.monitorWhatId).toEqual(MW_ID);
  });

  it('should disable and change text while waiting', fakeAsync(() => {
    spyOn(monitorService, 'startMonitor').and.callFake(mw => ({ ...mw, success$: EMPTY, failure$: EMPTY }));

    directive.kickoff();
    tick(2);

    fixture.detectChanges();

    return button
      .isDisabled()
      .then(disabled => expect(disabled).toBe(true))
      .then(() => button.getText())
      .then(txt => expect(txt).toEqual('Working...'));
  }));

  it('should be enabled and have original text after succeeding', fakeAsync(() => {
    spyOn(monitorService, 'startMonitor').and.callFake(mw => ({
      ...mw,
      success$: of({ ...mw, type: MonitorWhatResultType.Success }),
      failure$: EMPTY,
    }));

    directive.kickoff();
    tick(2);

    fixture.detectChanges();

    return button
      .isDisabled()
      .then(disabled => expect(disabled).toBe(false))
      .then(() => button.getText())
      .then(txt => expect(txt.toLowerCase()).toEqual('register'));
  }));

  it('should be enabled with original text and display toast after failing', fakeAsync(() => {
    spyOn(monitorService, 'startMonitor').and.callFake(mw => ({
      ...mw,
      success$: EMPTY,
      failure$: of({ ...mw, type: MonitorWhatResultType.Failure }),
    }));
    spyOn(toastService, 'show').and.stub();

    directive.kickoff();
    tick(2);

    fixture.detectChanges();

    return button
      .isDisabled()
      .then(disabled => expect(disabled).toBe(false))
      .then(() => button.getText())
      .then(txt => expect(txt.toLowerCase()).toEqual('register'))
      .then(() => expect(toastService.show).toHaveBeenCalled());
  }));
});
