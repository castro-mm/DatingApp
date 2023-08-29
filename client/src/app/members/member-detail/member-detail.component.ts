import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { FormsModule } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, FormsModule, TimeagoModule]
})
export class MemberDetailComponent implements OnInit {

    member: Member | undefined;
    images: GalleryItem[] = [];

    constructor(private memberService: MembersService, private route: ActivatedRoute) { }

    ngOnInit(): void {
        this.loadMember();
    }

    loadMember() {
        // get param from route. you can get the parameter via @Input directive, when you need the whole object, or via snapshot, when the parameter is a number or text.
        var username = this.route.snapshot.paramMap.get('username'); 
        if (username) {
            this.memberService.getMember(username).subscribe({
                next: (member) => {
                    this.member = member;
                    this.getImages();
                }
            });
        }
    }

    getImages() {
        if (!this.member) return;

        for (const photo of this.member.photos) {
            this.images.push(new ImageItem({src: photo.url, thumb: photo.url}));
        }
    }
}
