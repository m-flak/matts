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

import { Directive, Host, Self, Optional, Input, HostListener, OnInit } from "@angular/core";
import { MatIcon } from "@angular/material/icon";
import { MatInput } from "@angular/material/input";

@Directive({
    selector: '[cmpMatIconLink]',
    host: {
        '[style.cursor]': '"pointer"',
        '[attr.title]': 'hostTooltip'
    }
})
export class MatIconLinkDirective implements OnInit {
    private _uriPrefix = '';
    private _uri = '';

    hostTooltip: string = '';

    @Input()
    cmpMatIconLink = '';

    @Input()
    inputRef: MatInput | null | undefined;

    @Input()
    resumeDownloadHref = '';
    
    constructor(
        @Host() @Self() @Optional() public hostMatIcon: MatIcon | null
    ) {}
    
    ngOnInit(): void {
        if ( this.hostMatIcon !== null && (this.inputRef !== null && this.inputRef !== undefined) ) {
            if (this.cmpMatIconLink === 'email') {
                this.hostTooltip = 'Send Email';
                this.hostMatIcon.fontIcon = 'email';
                this._uriPrefix = 'mailto:';
                this._uri = this.inputRef.value;
            }
            else if (this.cmpMatIconLink === 'tel') {
                this.hostTooltip = 'Call';
                this.hostMatIcon.fontIcon = 'phone';
                this._uriPrefix = 'tel:';
                this._uri = this.inputRef.value;
            }
            else if (this.cmpMatIconLink === 'resume') {
                this.hostTooltip = 'Download Resume';
                this.hostMatIcon.fontIcon = 'folder_shared';
            }
        }
    }

    @HostListener('click')
    onHostClick(): void {
        if (this.cmpMatIconLink !== 'resume') {
            window.open(`${this._uriPrefix}${this._uri}`, '_blank');
        }
        else {
            window.open(this.resumeDownloadHref, '_blank');
        }
    }
}
