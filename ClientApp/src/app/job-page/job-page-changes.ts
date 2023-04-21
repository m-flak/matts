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
import { JobPageDataService } from "../services/job-page-data.service";

export type ChangeCommandData = any;
export type ChangeCommand = (serviceInstance: JobPageDataService, commandData: ChangeCommandData) => Promise<boolean>;

export class JobPageChanges {
    // True if successful
    private _changeCommand!: ChangeCommand;
    private _commandData!: ChangeCommandData;

    public sortTag: string = '';

    constructor(sortTag: string, changeCommand: ChangeCommand, commandData: ChangeCommandData) {
        this.sortTag = sortTag;
        this._changeCommand = changeCommand;
        this._commandData = commandData;
    }

    // True if successful
    async invokeCommand(usingService: JobPageDataService): Promise<boolean> {
        return await this._changeCommand(usingService, this._commandData);
    }
}
