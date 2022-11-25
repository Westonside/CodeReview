package com.codeReview.socialmedia.dto;


import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import javax.persistence.Id;

@Data
@Builder
@AllArgsConstructor
@NoArgsConstructor
public class PostResponse {
    private String uID;
    private String content;
}
