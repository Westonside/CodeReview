package com.codeReview.socialmedia.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.mongodb.core.mapping.Document;
import org.springframework.web.bind.annotation.RestController;

import javax.persistence.Id;

@Document(value = "post")
@AllArgsConstructor
@NoArgsConstructor
@Builder
@Data
public class Post {
    @Id
    private String pID;
    private String uID;
    private String content;
}
