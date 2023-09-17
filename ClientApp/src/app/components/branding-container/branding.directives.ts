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

import { Directive, ElementRef, Inject } from '@angular/core';

@Directive({
  selector: '[branding-with-brand]',
})
export class BrandingWithBrandDirective {
  constructor(@Inject(ElementRef) private element: ElementRef) {}

  setupBrand(value: string) {
    let tag: string = this.element.nativeElement.tagName ?? '';
    tag = tag.toLowerCase();

    if (tag.length > 0) {
      if (tag === 'img') {
        this.element.nativeElement.setAttribute('src', value);
      } else {
        this.element.nativeElement.innerText = value;
      }
    }
  }
}

@Directive({
  selector: '[branding-none-default]',
})
export class BrandingNoneDefaultDirective {
  constructor() {}
}
