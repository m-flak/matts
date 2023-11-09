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

export enum WSAuthEventTypes {
  NONE = 0,
  CLIENT_OAUTH_START = 1000,
  CLIENT_OAUTH_REQUEST_STATUS = 1001,
  SERVER_CONNECTION_ESTABLISHED = 2000,
  SERVER_OAUTH_STARTED = 2001,
  SERVER_OAUTH_PENDING = 2002,
  SERVER_OAUTH_COMPLETED = 2003,
  SERVER_OAUTH_ABORTFAIL = 2999,
}

export interface WSAuthMessage {
  type: WSAuthEventTypes;
  clientIdentity: string;
  data: any;
}
