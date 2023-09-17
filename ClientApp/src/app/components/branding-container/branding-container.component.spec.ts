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

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BrandingContainerComponent } from './branding-container.component';
import { ComponentsModule } from '../components.module';
import { Component, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { ConfigService } from 'src/app/services/config.service';
import { Configuration } from 'src/app/models';

@Component({
  selector: 'test-component',
  template: `<cmp-branding-container configEntry="coolBranding"><a [branding-none-default] href="_blank">Default</a><img [branding-with-brand] /></cmp-branding-container>`,
})
class TestComponent {}

const config: Configuration = {
  externalApis: {},
  branding: {},
};

const FakeConfigService = {
  config: config,
};

describe('BrandingContainerComponent', () => {
  let componentHost: DebugElement;
  let component: BrandingContainerComponent;
  let fixture: ComponentFixture<TestComponent>;

  let origConfiguration: Configuration = { ...config };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComponentsModule],
      declarations: [TestComponent],
      providers: [{ provide: ConfigService, useValue: FakeConfigService }],
      teardown: { destroyAfterEach: false },
    }).compileComponents();

    fixture = TestBed.createComponent(TestComponent);
    componentHost = fixture.debugElement.query(By.directive(BrandingContainerComponent));
    component = componentHost.injector.get(BrandingContainerComponent);
    fixture.detectChanges();
    component.ngOnInit();
    fixture.detectChanges();
  });

  beforeEach(() => {
    FakeConfigService.config = { ...origConfiguration };
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show the anchor instead when branding is not present', () => {
    expect(component.hasBranding).toBeFalsy();

    const a = componentHost.queryAll(By.css('a'));
    const img = componentHost.queryAll(By.css('img'));

    expect(a.length).toEqual(1);
    expect(img.length).toEqual(0);
  });

  it('should show the image instead when branding is present', () => {
    FakeConfigService.config.branding.coolBranding = 'https://site.com/brand.png';
    component.ngOnInit();
    component.ngAfterContentInit();
    fixture.detectChanges();

    expect(component.hasBranding).toBeTruthy();

    const a = componentHost.queryAll(By.css('a'));
    const img = componentHost.queryAll(By.css('img'));

    expect(a.length).toEqual(0);
    expect(img.length).toEqual(1);

    expect(img[0].nativeElement.getAttribute('src')).toEqual('https://site.com/brand.png');
  });
});
