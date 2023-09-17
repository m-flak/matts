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
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ComponentsModule } from '../components.module';
import { By } from '@angular/platform-browser';
import { BrandingNoneDefaultDirective, BrandingWithBrandDirective } from './branding.directives';

@Component({
  selector: 'test-component',
  template: `<div><p id="text" [branding-with-brand]>No Flavor</p><img id="image" [branding-with-brand] /><a [branding-none-default] href="_blank">Boring stock link</a></div>`,
})
class TestComponent {}

describe('BrandingWithBrandDirective and BrandingNoneDefaultDirective', () => {
  let fixture: ComponentFixture<TestComponent>;
  let brandedTextDirective: BrandingWithBrandDirective;
  let brandedImageDirective: BrandingWithBrandDirective;
  let unbrandedDirective: BrandingNoneDefaultDirective;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TestComponent],
      imports: [ComponentsModule],
      teardown: { destroyAfterEach: false },
    }).compileComponents();

    fixture = TestBed.createComponent(TestComponent);
    fixture.detectChanges();

    await fixture.whenStable();
    const brandedText = fixture.debugElement.query(By.css('#text'));
    const brandedImage = fixture.debugElement.query(By.css('#image'));
    const unbranded = fixture.debugElement.query(By.directive(BrandingNoneDefaultDirective));

    brandedTextDirective = brandedText.injector.get(BrandingWithBrandDirective);
    brandedImageDirective = brandedImage.injector.get(BrandingWithBrandDirective);
    unbrandedDirective = unbranded.injector.get(BrandingNoneDefaultDirective);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
    expect(brandedTextDirective).toBeTruthy();
    expect(brandedImageDirective).toBeTruthy();
    expect(unbrandedDirective).toBeTruthy();
  });

  it('should update the innerText for the "p"', () => {
    brandedTextDirective.setupBrand('A ton of flavor!');
    fixture.detectChanges();
    const p = fixture.debugElement.nativeElement.querySelector('#text');
    expect(p.innerText).toEqual('A ton of flavor!');
  });

  it('should update the src for the "img"', () => {
    brandedImageDirective.setupBrand('https://site.com/brand.png');
    fixture.detectChanges();
    const img = fixture.debugElement.nativeElement.querySelector('#image');
    const srcAttr: string | null = img.getAttribute('src');
    expect(srcAttr).not.toBeNull();
    expect(srcAttr).toEqual('https://site.com/brand.png');
  });
});
