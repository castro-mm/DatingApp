import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of, take } from 'rxjs';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { User } from '../_models/user';
import { LikeParams } from '../_models/likeParams';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
    providedIn: 'root'
})
export class MembersService {

    baseApiUrl = environment.apiUrl;
    members: Member[] = [];
    memberCache = new Map();
    user: User | undefined;
    userParams: UserParams | undefined;

    constructor(private http: HttpClient, private accountService: AccountService) { 

        this.accountService.currentUser$.pipe(take(1)).subscribe({
            next: (user) => {
                if (user) {
                    this.userParams = new UserParams(user);
                    this.user = user;    
                }
            }
        })
    }

    getUserParams() {
        return this.userParams;
    }

    setUserParams(userParams: UserParams) {
        this.userParams = userParams;
    }

    resetUserParams() {
        if(this.user) {
            this.userParams = new UserParams(this.user);
            return this.userParams;
        }
        return;
    }

    getMembers(userParams: UserParams) {

        const response = this.memberCache.get(Object.values(userParams).join('-'));

        if (response) return of(response);

        let params = getPaginationHeaders(userParams);
      
        return getPaginatedResult<Member[]>(this.baseApiUrl + 'users/', params, this.http).pipe(
            map(response => {
                this.memberCache.set(Object.values(userParams).join('-'), response);
                return response;
            })
        );
    }    

    getMember(username: string) {
        const member = [...this.memberCache.values()]
            .reduce((arr, elem) => arr.concat(elem.result), [])
            .find((member: Member) => member.userName === username);

        if(member) return of(member);
        
        return this.http.get<Member>(this.baseApiUrl + 'users/' + username);
    }

    updateMember(member: Member) {
        return this.http.put(this.baseApiUrl + 'users', member).pipe(
            map(() => {
                const index = this.members.indexOf(member);
                this.members[index] = {...this.members[index], ...member};
            })
        );
    }

    setMainPhoto(photoId: number) { 
        return this.http.put(this.baseApiUrl + 'users/set-main-photo/' + photoId, {});
    }

    deletePhoto(photoId: number) {
        return this.http.delete(this.baseApiUrl + 'users/delete-photo/' + photoId);
    }

    addLike(username: string) {
        return this.http.post(this.baseApiUrl + 'likes/' + username, {});
    }

    getLikes(likeParams: LikeParams) {

        let params = getPaginationHeaders(likeParams);
      
        return getPaginatedResult<Member[]>(this.baseApiUrl + 'likes/', params, this.http);

        //return this.http.get<Member[]>(this.baseApiUrl + 'likes/?predicate=' + predicate);
    }    
}
