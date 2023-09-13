import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
    selector: 'app-photo-management',
    templateUrl: './photo-management.component.html',
    styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
    photos?: Photo[] = [];    

    constructor(private adminService: AdminService, private toastr: ToastrService) { }
    
    ngOnInit(): void {
        this.getPhotosForApproval();
    }

    getPhotosForApproval() {
        return this.adminService.getPhotosForApproval().subscribe({
            next: (photos) => {
                if (photos) this.photos = photos;
            }
        })
    }

    approvePhoto(photoId: number){
        return this.adminService.approvePhoto(photoId).subscribe({
            next: () => {                
                this.getPhotosForApproval();
                this.toastr.show('Photo Approved!')
            },
            error: (error) => {
                this.toastr.error(`Failed to reject photo. Details: ${error}`);
            }
        })
    }

    rejectPhoto(photoId: number){
        return this.adminService.rejectPhoto(photoId).subscribe({
            next: () => {
                this.getPhotosForApproval();
                this.toastr.show('Photo Rejected!');
            },
            error: (error) => {
                this.toastr.error(`Failed to reject photo. Details: ${error}`);
            }
        })
    }
}
