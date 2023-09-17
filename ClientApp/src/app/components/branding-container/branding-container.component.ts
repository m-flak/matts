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


import { AfterContentInit, Component, ContentChild, Input, OnInit } from '@angular/core';
import { ConfigService } from 'src/app/services/config.service';
import { BrandingWithBrandDirective } from './branding.directives';

@Component({
  selector: 'cmp-branding-container',
  templateUrl: './branding-container.component.html'
})
export class BrandingContainerComponent implements OnInit, AfterContentInit {
  @Input()
  configEntry!: string;

  hasBranding = false;
  brandingValue: string = '';

  @ContentChild(BrandingWithBrandDirective)
  brandedChild!: BrandingWithBrandDirective;

  constructor(private configService: ConfigService) { }

  ngOnInit(): void {
    if ( this.configService.config.branding !== undefined
      && this.configService.config.branding !== null
      && this.configService.config.branding.hasOwnProperty(this.configEntry)
    ) {
      this.hasBranding = true;
      this.brandingValue = this.configService.config.branding[this.configEntry];
    }
  }

  ngAfterContentInit() {
    if (this.hasBranding) {
      this.brandedChild.setupBrand(this.brandingValue);
    }
  }
}
