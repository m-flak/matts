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
namespace matts.Constants;

public sealed class RelationshipConstants
{
    public const string HAS_APPLIED_TO = "HAS_APPLIED_TO";
    public const string IS_INTERVIEWING_FOR = "IS_INTERVIEWING_FOR";
    public const string IS_USER_FOR = "IS_USER_FOR";
    public const string INTERVIEWING_WITH = "INTERVIEWING_WITH";

    // Constants class
    private RelationshipConstants() {}
}