import { User } from "./user.model";

export interface UserRegistration extends User {
    fullName: string;
    companyName: string | null;
}
