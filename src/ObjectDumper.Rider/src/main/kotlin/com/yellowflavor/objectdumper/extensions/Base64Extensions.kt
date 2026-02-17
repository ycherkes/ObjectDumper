package com.yellowflavor.objectdumper.extensions

import java.util.*

object Base64Extensions {
    fun String.toBase64(): String {
        return Base64.getEncoder().encodeToString(this.toByteArray(Charsets.UTF_8))
    }
    
    fun String.fromBase64(): String {
        return String(Base64.getDecoder().decode(this), Charsets.UTF_8)
    }
}
